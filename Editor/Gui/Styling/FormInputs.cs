﻿using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using T3.Editor.Gui.InputUi;
using T3.Editor.Gui.Interaction;
using T3.Editor.Gui.UiHelpers;

namespace T3.Editor.Gui.Styling
{
    /// <summary>
    /// A set of custom widgets that allow to quickly draw dialogs with a more "traditional" labels on the left side of input fields.
    /// It also provides a bunch of helper methods for minimal layout control. 
    /// </summary>
    public static class FormInputs
    {
        public static void BeginFrame()
        {
            ResetIndent();
        }
        
        public static void AddSectionHeader(string label)
        {
            AddVerticalSpace(10);
            ImGui.PushFont(Fonts.FontLarge);
            ImGui.Text(label);
            ImGui.PopFont();
        }

        public static bool BeginGroup(string label)
        {
            var shouldBeOpenByDefault = !label.EndsWith("...");

            AddVerticalSpace(5);
            ImGui.PushStyleColor(ImGuiCol.Text, UiColors.TextMuted.Rgba);

            var id = ImGui.GetID(label);
            if (shouldBeOpenByDefault && !_openedGroups.Contains(id))
            {
                ImGui.SetNextItemOpen(true);
                _openedGroups.Add(id);
            }

            var isOpen = ImGui.TreeNode(label);
            ImGui.PopStyleColor();
            if(isOpen)
                ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 0);
            
            return isOpen;
        }

        private static HashSet<uint> _openedGroups = new();

        public static void EndGroup()
        {
            ImGui.PopStyleVar();
            ImGui.TreePop();
        }

        public static bool AddInt(string label,
                                  ref int value,
                                  int min = int.MinValue,
                                  int max = int.MaxValue,
                                  float scale = 1,
                                  string tooltip = null,
                                  int defaultValue = NotADefaultValue)
        {
            DrawInputLabel(label);

            ImGui.PushID(label);

            var hasReset = defaultValue != NotADefaultValue;

            var size = GetAvailableInputSize(tooltip, hasReset);
            var result = SingleValueEdit.Draw(ref value, size, min, max, true, scale);
            ImGui.PopID();

            AppendTooltip(tooltip);
            if (AppendResetButton(hasReset, label))
            {
                value = defaultValue;
                result |= InputEditStateFlags.ModifiedAndFinished;
            }

            var modified = (result & InputEditStateFlags.Modified) != InputEditStateFlags.Nothing;
            return modified;
        }

        private const float DefaultFadeAlpha = 0.7f;

        public static bool AddFloat(string label,
                                    ref float value,
                                    float min = float.NegativeInfinity,
                                    float max = float.PositiveInfinity,
                                    float scale = 0.01f,
                                    bool clamp = false,
                                    string tooltip = null,
                                    float defaultValue = float.NaN)
        {
            var hasReset = !float.IsNaN(defaultValue);
            var isDefault = hasReset && Math.Abs(value - defaultValue) < 0.0001f;
            if (isDefault)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, DefaultFadeAlpha * ImGui.GetStyle().Alpha);
            }

            DrawInputLabel(label);
            var size = GetAvailableInputSize(tooltip, hasReset);

            ImGui.PushID(label);
            var result = SingleValueEdit.Draw(ref value, size, min, max, clamp, scale);
            ImGui.PopID();

            AppendTooltip(tooltip);
            if (AppendResetButton(hasReset && !isDefault, label))
            {
                value = defaultValue;
                result |= InputEditStateFlags.ModifiedAndFinished;
            }

            if (isDefault)
            {
                ImGui.PopStyleVar();
            }

            var modified = (result & InputEditStateFlags.Modified) != InputEditStateFlags.Nothing;
            return modified;
        }

        public static bool AddEnumDropdown<T>(ref T selectedValue, string label, string tooltip = null) where T : struct, Enum, IConvertible, IFormattable
        {
            DrawInputLabel(label);

            var inputSize = GetAvailableInputSize(tooltip, false, true);
            ImGui.SetNextItemWidth(inputSize.X);

            var names = Enum.GetNames<T>();
            var index = 0;
            var selectedIndex = 0;

            foreach (var n in names)
            {
                if (n == selectedValue.ToString())
                    selectedIndex = index;

                index++;
            }

            var modified = ImGui.Combo($"##dropDown{typeof(T)}{label}", ref selectedIndex, names, names.Length, names.Length);
            if (modified)
            {
                selectedValue = Enum.GetValues<T>()[selectedIndex];
            }

            AppendTooltip(tooltip);

            return modified;
        }

        public static bool AddDropdown(ref string selectedValue, IEnumerable<string> values, string label, string tooltip = null)
        {
            DrawInputLabel(label);

            var inputSize = GetAvailableInputSize(tooltip, false, true);
            ImGui.SetNextItemWidth(inputSize.X);

            var modified = false;
            if (ImGui.BeginCombo("##SelectTheme", UserSettings.Config.ColorThemeName, ImGuiComboFlags.HeightLarge))
            {
                foreach (var value in values)
                {
                    if (value == null)
                        continue;

                    var isSelected = value == selectedValue;
                    if (!ImGui.Selectable($"{value}", isSelected, ImGuiSelectableFlags.DontClosePopups))
                        continue;

                    ImGui.CloseCurrentPopup();
                    selectedValue = value;
                    modified = true;
                }

                ImGui.EndCombo();
            }

            AppendTooltip(tooltip);
            return modified;
        }
        
        
        public static bool AddSegmentedButton<T>(ref T selectedValue, string label) where T : struct, Enum
        {
            DrawInputLabel(label);

            var modified = false;
            var selectedValueString = selectedValue.ToString();
            var isFirst = true;
            foreach (var value in Enum.GetValues<T>())
            {
                var name = Enum.GetName(value);
                if (!isFirst)
                {
                    ImGui.SameLine();
                }

                var isSelected = selectedValueString == value.ToString();
                var clicked = DrawSelectButton(name, isSelected);

                if (clicked)
                {
                    modified = true;
                    selectedValue = value;
                }

                isFirst = false;
            }

            return modified;
        }

        private static bool DrawSelectButton(string name, bool isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, isSelected ? UiColors.BackgroundActive.Rgba : UiColors.BackgroundButton.Rgba);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, isSelected ? UiColors.BackgroundActive.Rgba : UiColors.BackgroundButton.Rgba);
            ImGui.PushStyleColor(ImGuiCol.Text, isSelected ? UiColors.BackgroundFull.Rgba : UiColors.ForegroundFull.Rgba);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, UiColors.BackgroundActive.Rgba);

            var clicked = ImGui.Button(name);
            ImGui.PopStyleColor(4);
            return clicked;
        }

        private const string NoDefaultString = "_";

        /// <summary>
        /// Draws string input or file picker. 
        /// </summary>
        public static bool AddStringInput(string label,
                                          ref string value,
                                          string placeHolder = null,
                                          string warning = null,
                                          string tooltip = null,
                                          string defaultValue = NoDefaultString)
        {
            var hasDefault = defaultValue != NoDefaultString;
            var isDefault = hasDefault && value == defaultValue;

            if (isDefault)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, DefaultFadeAlpha);
            }

            DrawInputLabel(label);
            var wasNull = value == null;
            if (wasNull)
                value = string.Empty;

            var inputSize = GetAvailableInputSize(tooltip, false, true);
            ImGui.SetNextItemWidth(inputSize.X);
            var modified = ImGui.InputText("##" + label, ref value, 1000);
            if (!modified && wasNull)
                value = null;

            AppendTooltip(tooltip);

            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(placeHolder))
            {
                var drawList = ImGui.GetWindowDrawList();
                var minPos = ImGui.GetItemRectMin();
                var maxPos = ImGui.GetItemRectMax();
                drawList.PushClipRect(minPos, maxPos);
                drawList.AddText(minPos + new Vector2(8, 3), UiColors.ForegroundFull.Fade(0.25f), placeHolder);
                drawList.PopClipRect();
            }

            if (isDefault)
            {
                ImGui.PopStyleVar();
            }

            if (AppendResetButton(hasDefault && !isDefault, label))
            {
                value = defaultValue;
                modified = true;
            }

            DrawWarningBelowField(warning);

            return modified;
        }

        /// <summary>
        /// Draws string input or file picker. 
        /// </summary>
        public static bool AddFilePicker(string label,
                                         ref string value,
                                         string placeHolder = null,
                                         string warning = null,
                                         FileOperations.FilePickerTypes showFilePicker = FileOperations.FilePickerTypes.None)
        {
            DrawInputLabel(label);

            var isFilePickerVisible = showFilePicker != FileOperations.FilePickerTypes.None;
            float spaceForFilePicker = isFilePickerVisible ? 30 : 0;
            var inputSize = GetAvailableInputSize(null, false, true, spaceForFilePicker);
            ImGui.SetNextItemWidth(inputSize.X);

            var wasNull = value == null;
            if (wasNull)
                value = string.Empty;

            var modified = ImGui.InputText("##" + label, ref value, 1000);
            if (!modified && wasNull)
                value = null;

            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(placeHolder))
            {
                var drawList = ImGui.GetWindowDrawList();
                var minPos = ImGui.GetItemRectMin();
                var maxPos = ImGui.GetItemRectMax();
                drawList.PushClipRect(minPos, maxPos);
                drawList.AddText(minPos + new Vector2(8, 3), UiColors.ForegroundFull.Fade(0.25f), placeHolder);
                drawList.PopClipRect();
            }

            if (isFilePickerVisible)
            {
                modified |= FileOperations.DrawFileSelector(showFilePicker, ref value);
            }

            DrawWarningBelowField(warning);
            return modified;
        }

        public static bool AddCheckBox(string label,
                                       ref bool value,
                                       string tooltip = null,
                                       bool? defaultValue = null)
        {
            var hasDefault = defaultValue != null;
            var isDefault = hasDefault && value == (bool)defaultValue;

            if (isDefault)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, DefaultFadeAlpha);
            }

            ImGui.SetCursorPosX(MathF.Max(LeftParameterPadding, 0) + 20);
            var modified = ImGui.Checkbox(label, ref value);

            AppendTooltip(tooltip);
            if (isDefault)
            {
                ImGui.PopStyleVar();
            }

            if (AppendResetButton(hasDefault && !isDefault, label))
            {
                value = defaultValue ?? false;
                modified = true;
            }

            return modified;
        }
        
        
        
        public static void AddHint(string label)
        {
            if (string.IsNullOrEmpty(label))
                return;

            AddVerticalSpace(5);
            ApplyIndent();
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 20));

            AddIcon(Icon.Hint);

            ImGui.SameLine();
            ImGui.TextWrapped(label);
            //ImGui.Indent(-13);
            ImGui.PopStyleVar(2);
        }

        public static void AddVerticalSpace(float size = 10)
        {
            ImGui.Dummy(new Vector2(1, size * T3Ui.UiScaleFactor));
        }

        #region layout helpers
        public static void SetIndent(float newIndent)
        {
            _paramIndent = newIndent;
        }

        public static void ResetIndent()
        {
            _paramIndent = DefaultParameterIndent;
        }

        public static void ApplyIndent()
        {
            ImGui.SetCursorPosX(LeftParameterPadding + ParameterSpacing);
        }

        public static void SetWidth(float ratio)
        {
            _widthRatio = ratio;
        }

        public static void ResetWidth()
        {
            _widthRatio = 1;
        }

        public static void DrawInputLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
                return;

            var labelSize = ImGui.CalcTextSize(label);
            var p = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(MathF.Max(LeftParameterPadding - labelSize.X, 0) + 10);
            ImGui.AlignTextToFramePadding();

            ImGui.TextUnformatted(label);
            ImGui.SetCursorPos(p);

            ImGui.SameLine();
            ImGui.SetCursorPosX(LeftParameterPadding + ParameterSpacing);
        }

        private static void DrawWarningBelowField(string warning)
        {
            if (string.IsNullOrEmpty(warning))
                return;

            ImGui.SetCursorPosX(MathF.Max(LeftParameterPadding, 0) + 20);
            ImGui.PushFont(Fonts.FontSmall);
            ImGui.PushStyleColor(ImGuiCol.Text, UiColors.StatusError.Rgba);
            ImGui.TextUnformatted(warning);
            ImGui.PopStyleColor();
            ImGui.PopFont();
        }
        #endregion

        #region internal helpers
        private static Vector2 GetAvailableInputSize(string tooltip, bool hasReset, bool fillWidth = false, float rightPadding = 0)
        {
            var toolWidth = 20f * T3Ui.UiScaleFactor;
            var sizeForResetToDefault = hasReset ? toolWidth : 0;
            var sizeForTooltip = !string.IsNullOrEmpty(tooltip) ? toolWidth : 0;

            var availableWidth = fillWidth ? ImGui.GetContentRegionAvail().X * _widthRatio : 200;

            var vector2 = new Vector2(availableWidth
                                      - 20
                                      - rightPadding
                                      - sizeForResetToDefault
                                      - sizeForTooltip,
                                      ImGui.GetFrameHeight());
            return vector2;
        }

        private static void AppendTooltip(string tooltip)
        {
            if (string.IsNullOrEmpty(tooltip))
                return;

            ImGui.SameLine();

            ImGui.PushFont(Icons.IconFont);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(" " + (char)Icon.Help);

            ImGui.PopStyleVar(2);
            ImGui.PopFont();

            //CustomComponents.TooltipForLastItem(tooltip, null, false);
            if (!ImGui.IsItemHovered())
                return;

            // Tooltip
            ImGui.PushStyleColor(ImGuiCol.PopupBg, UiColors.BackgroundFull.Rgba);
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1);
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(300);
            ImGui.TextUnformatted(tooltip);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar(1);
        }

        private static bool AppendResetButton(bool hasReset, string id)
        {
            if (!hasReset)
                return false;

            ImGui.SameLine();
            ImGui.PushID(id);
            var clicked = CustomComponents.IconButton(Icon.Revert, Vector2.One * ImGui.GetFrameHeight());
            ImGui.PopID();
            return clicked;
        }

        private static void AddIcon(Icon icon)
        {
            ImGui.PushFont(Icons.IconFont);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

            ImGui.TextUnformatted("" + (char)(icon));

            ImGui.PopFont();
            ImGui.PopStyleVar(2);
        }
        #endregion

        private const int NotADefaultValue = Int32.MinValue;

        private const float DefaultParameterIndent = 170;
        private static float _paramIndent = DefaultParameterIndent;
        private static float _widthRatio = 1;
        private static float LeftParameterPadding => _paramIndent * T3Ui.UiScaleFactor;
        public static float ParameterSpacing => 20 * T3Ui.UiScaleFactor;


    }
}
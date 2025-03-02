﻿using System.Collections.Generic;
using ImGuiNET;
using T3.Editor.Gui.Graph.Helpers;
using T3.Editor.Gui.Graph.Interaction;
using T3.Editor.Gui.Graph.Modification;
using T3.Editor.Gui.Styling;
using T3.Editor.Gui.UiHelpers;

namespace T3.Editor.Gui.Graph.Dialogs
{
    public class RenameSymbolDialog : ModalDialog
    {
        public void Draw(List<SymbolChildUi> selectedChildUis, ref string name)
        {
            if (BeginDialog("Rename symbol"))
            {
                ImGui.PushFont(Fonts.FontSmall);
                ImGui.TextUnformatted("Name");
                ImGui.PopFont();

                ImGui.SetNextItemWidth(150);

                if (ImGui.IsWindowAppearing())
                    ImGui.SetKeyboardFocusHere();

                ImGui.InputText("##name", ref name, 255);

                CustomComponents.HelpText("This is a C# class. It must be unique and\nnot include spaces or special characters");
                ImGui.Spacing();

                if (CustomComponents.DisablableButton("Rename", GraphUtils.IsNewSymbolNameValid(name)))
                {
                    SymbolNaming.RenameSymbol(selectedChildUis[0].SymbolChild.Symbol, name);
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                EndDialogContent();
            }
            EndDialog();
        }
    }
}
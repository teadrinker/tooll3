﻿using System.Numerics;
using ImGuiNET;
using T3.Core.Operator;
using T3.Editor.Gui.Styling;
using T3.Editor.Gui.UiHelpers;

namespace T3.Editor.Gui.Graph.Dialogs
{
    public class EditSymbolDescriptionDialog : ModalDialog
    {
        public void Draw(Symbol operatorSymbol)
        {
            DialogSize = new Vector2(1100, 700);
            
            if (BeginDialog("Edit description"))
            {
                var symbolUi = SymbolUiRegistry.Entries[operatorSymbol.Id];
                var desc = symbolUi.Description ?? string.Empty;
                
                ImGui.PushFont(Fonts.FontLarge);
                ImGui.Text(symbolUi.Symbol.Name);
                ImGui.PopFont();
                
                if (ImGui.IsWindowAppearing())
                    ImGui.SetKeyboardFocusHere();

                ImGui.InputTextMultiline("##name", ref desc, 2000, new Vector2(-1,400), ImGuiInputTextFlags.None);
                symbolUi.Description = desc;
                
                if (ImGui.Button("Close"))
                {
                    ImGui.CloseCurrentPopup();
                }
                
                EndDialogContent();
            }
            EndDialog();
        }
    }
}
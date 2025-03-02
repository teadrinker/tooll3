﻿using System;
using System.IO;
using System.Linq;
using ImGuiNET;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Core.Operator.Interfaces;
using T3.Editor.Gui.ChildUi.WidgetUi;
using T3.Editor.Gui.UiHelpers;

namespace T3.Editor.Gui.ChildUi
{
    public static class DescriptiveUi
    {
        public static SymbolChildUi.CustomUiResult DrawChildUi(Instance instance, ImDrawListPtr drawList, ImRect area)
        {
            if (instance is not IDescriptiveFilename descriptiveGraphNode )
                return SymbolChildUi.CustomUiResult.None;
            
            drawList.PushClipRect(area.Min, area.Max, true);
            
            // Label if instance has title
            var symbolChild = instance.Parent.Symbol.Children.Single(c => c.Id == instance.SymbolChildId);
            
            WidgetElements.DrawSmallTitle(drawList, area, !string.IsNullOrEmpty(symbolChild.Name) ? symbolChild.Name : symbolChild.Symbol.Name);

            var slot = descriptiveGraphNode.GetSourcePathSlot();
            var filePath = slot?.TypedInputValue?.Value;
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    filePath = Path.GetFileName(filePath);
                }
                catch (Exception e)
                {
                    Log.Debug("Can't get filepath for output " + e.Message);
                }
            }
            
            WidgetElements.DrawPrimaryValue(drawList, area, filePath);
            
            drawList.PopClipRect();
            return SymbolChildUi.CustomUiResult.Rendered | SymbolChildUi.CustomUiResult.PreventInputLabels;
        }
    }
}
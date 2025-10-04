using System;
using System.Collections.Generic;
using System.Text;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Common.BL
{
    public class SelectionManager
    {
        private List<ISelectionItem> data;
        private SelectionMode selectionMode;

        public SelectionManager(List<ISelectionItem> data, SelectionMode mode)
        {
            this.data = data;
            this.selectionMode = mode;
        }

        public void Select(ISelectionItem selectionItem)
        {
            bool selectedvalue = selectionItem.IsSelected;
            if (selectionItem != null)
            {
                if (selectionMode == SelectionMode.Single )
                {
                    DeselectAll();
                    if (selectedvalue == false)
                    {
                        selectionItem.IsSelected = true; // Select the specified ISelectionItem
                    }
                }
                else if (selectionMode == SelectionMode.Multiple)
                {
                    selectionItem.IsSelected = !selectionItem.IsSelected;
                }
            } 
            else
            {
                DeselectAll();
            }
        }
        public void DeselectAll()
        {
            foreach (var dataISelectionItem in data)
            {
                dataISelectionItem.IsSelected = false; // Deselect all ISelectionItems for single select
            }
        }
        
        public void selectAll()
        {
            foreach (var dataISelectionItem in data)
            {
                dataISelectionItem.IsSelected = true; // select all ISelectionItems for single select
            }
        }

        public bool IsSelected(ISelectionItem ISelectionItem)
        {
            return ISelectionItem.IsSelected;
        }

        public List<ISelectionItem> GetSelectedSelectionItems()
        {
            return data.FindAll(ISelectionItem => ISelectionItem.IsSelected);
        }
        
        public int GetSelectionItemIndex()
        {
            return data.FindIndex(ISelectionItem => ISelectionItem.IsSelected);
        }
        public List<ISelectionItem> GetAllData()
        {
            return data;
        }
    }
}

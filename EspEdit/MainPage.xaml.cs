using EspEdit.ViewModels;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace EspEdit;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }



    //private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    if (RecordsView is null)
    //    {
    //        return;
    //    }

    //    ICommand selectionChangedCommand = RecordsView.SelectionChangedCommand;
    //    if (selectionChangedCommand != null)
    //    {
    //        //object selectionChangedCommandParameter = RecordsView.SelectionChangedCommandParameter;
    //        IList<object> selectedItems = RecordsView.SelectedItems;

    //        if (selectedItems is IList<object> list)
    //        {
    //            if (selectionChangedCommand.CanExecute(list))
    //            {
    //                selectionChangedCommand.Execute(list);
    //            }
    //        }
    //    }
    //}
}


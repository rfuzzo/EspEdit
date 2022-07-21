using Microsoft.Maui.Controls;
using System.Windows.Input;
using Tes3Json.Services;
using Tes3Json.ViewModels;

namespace EspEdit;

public partial class MainPage : ContentPage
{

    public MainPage()
    {


        InitializeComponent();


        BindingContext = App.Current.Services.GetService<MainPageViewModel>();

        // todo refactor this
        fileMenu.BindingContext = ViewModel;
        editMenu.BindingContext = ViewModel;
        viewMenu.BindingContext = ViewModel;
    }

    public MainPageViewModel ViewModel => (MainPageViewModel)BindingContext;

    private void RecordsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //await scrollView.ScrollToAsync(0, 0, true);

        Rect x = recordEditor.Bounds;
        recordEditor.Layout(rightGrid.Bounds);


        //scrollView.Layout(rightGrid.Bounds);



    }

    private void scrollView_Scrolled(object sender, ScrolledEventArgs e)
    {

    }

    private void MenuExit_Clicked(object sender, EventArgs e) => Application.Current.Quit();

    private void MenuChangeTheme_Clicked(object sender, EventArgs e)
    {
        switch (Application.Current.UserAppTheme)
        {
            case AppTheme.Unspecified:
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            case AppTheme.Light:
                Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            case AppTheme.Dark:
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            default:
                break;
        }
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton imageButton && imageButton.BindingContext is RecordViewModel viewModel && viewModel is not null)
        {
            ViewModel.DeleteRecordCommand.Execute(viewModel);
        }
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


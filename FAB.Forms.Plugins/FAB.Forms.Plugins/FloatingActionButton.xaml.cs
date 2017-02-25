using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace FAB.Forms.Plugins
{
  [Xamarin.Forms.ContentProperty("PageContent")]
  public partial class FloatingActionButton : ContentView
  {
    public static readonly BindableProperty IsButtonVisibleProperty = BindableProperty.CreateAttached(
          propertyName: "IsButtonVisible",
          returnType: typeof(bool),
          declaringType: typeof(FloatingActionButton),
          defaultValue: true,
          propertyChanged: OnButtonVisibleChanged);

    public ScrollView IsButtonVisible
    {
      get
      {
        return (ScrollView)GetValue(IsButtonVisibleProperty);
      }
      set
      {
        SetValue(IsButtonVisibleProperty, value);
      }
    }

    public static readonly BindableProperty MainButtonBackgroundColorProperty = BindableProperty.CreateAttached(
          propertyName: "MainButtonBackgroundColor",
          returnType: typeof(Color),
          declaringType: typeof(FloatingActionButton),
          defaultValue: Color.Aqua);

    public Color MainButtonBackgroundColor
    {
      get
      {
        return (Color)GetValue(MainButtonBackgroundColorProperty);
      }
      set
      {
        SetValue(MainButtonBackgroundColorProperty, value);
      }
    }

    public static readonly BindableProperty MainButtonMenuImageSourceProperty = BindableProperty.CreateAttached(
          propertyName: "MainButtonMenuImageSource",
          returnType: typeof(string),
          declaringType: typeof(FloatingActionButton),
          defaultValue: null);

    public string MainButtonMenuImageSource
    {
      get
      {
        return (string)GetValue(MainButtonMenuImageSourceProperty);
      }
      set
      {
        SetValue(MainButtonMenuImageSourceProperty, value);
      }
    }

    private ObservableCollection<FloatingActionButtonMenuItem> _menuItems = new ObservableCollection<FloatingActionButtonMenuItem>();

    public ObservableCollection<FloatingActionButtonMenuItem> MenuItems
    {
      get
      {
        return _menuItems;
      }
      set
      {
        _menuItems = value;
        BuildMenu();
      }
    }

    /// <summary>
    /// Construction du menu du FAB
    /// </summary>
    private void BuildMenu()
    {
      EmptyMenuContainer();

      if (MenuItems != null && MenuItems.Any())
      {
        foreach (var item in MenuItems)
        {
          MenuContainer.Children.Add(CreateMenuItem(item));
        }
      }
    }

    private RoundedTitledBoxView CreateMenuItem(FloatingActionButtonMenuItem item)
    {
      RoundedTitledBoxView menuItem = new RoundedTitledBoxView(item.ItemId, AsyncFoldDownMenuCommand);

      // on défini le BindingContext du bouton pour que les bindings fonctionnent sinon ce n'est pas le cas...
      item.BindingContext = this.BindingContext;

      menuItem.Title = item.Title;
      menuItem.ShowTitle = item.ShowTitle;
      menuItem.ButtonSize = ButtonSize.Mini;
      menuItem.ButtonBackgroundColor = item.ButtonBackgroundColor;
      menuItem.MenuImageSource = item.MenuImageSource;
      menuItem.Command = item.ButtonCommand;

      item.PropertyChanged += FABMenuItemPropertyChanged;

      // on déplace les boutons pour qu'ils soient aérés (et si c'est un mini on le centre par rapport aux autres)
      if (menuItem.ButtonSize == ButtonSize.Mini)
      {
        menuItem.SetValue(RoundedTitledBoxView.MarginProperty, new Thickness(10, 0, 10, 10));
      }
      else
      {
        menuItem.SetValue(RoundedTitledBoxView.MarginProperty, new Thickness(10, 0, 0, 10));
      }

      return menuItem;
    }

    private void FABMenuItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is FloatingActionButtonMenuItem)
      {
        FloatingActionButtonMenuItem menuItemChanged = (sender as FloatingActionButtonMenuItem);

        RoundedTitledBoxView elementInMenuContainer = (RoundedTitledBoxView)MenuContainer.Children.FirstOrDefault(x => (x is RoundedTitledBoxView) && (x as RoundedTitledBoxView).ItemId == menuItemChanged.ItemId);
        if (elementInMenuContainer != null)
        {
          // si possible, on ne mets à jour que la propriété qui a changée, sinon on recrée tout le bouton !
          if (e.PropertyName == nameof(FloatingActionButtonMenuItem.Title))
          {
            elementInMenuContainer.Title = menuItemChanged.Title;
          }
          else if (e.PropertyName == nameof(FloatingActionButtonMenuItem.ButtonBackgroundColor))
          {
            elementInMenuContainer.ButtonBackgroundColor = menuItemChanged.ButtonBackgroundColor;
          }
          else if (e.PropertyName == nameof(FloatingActionButtonMenuItem.MenuImageSource))
          {
            elementInMenuContainer.MenuImageSource = menuItemChanged.MenuImageSource;
          }
          else if (e.PropertyName == nameof(FloatingActionButtonMenuItem.ButtonCommand))
          {
            elementInMenuContainer.Command = menuItemChanged.ButtonCommand;
          }
          else
          {
            var position = MenuContainer.Children.IndexOf(elementInMenuContainer);

            menuItemChanged.PropertyChanged -= FABMenuItemPropertyChanged;
            MenuContainer.Children.Remove(elementInMenuContainer);
            MenuContainer.Children.Insert(position, CreateMenuItem(menuItemChanged));
          }
        }
      }
    }

    public static readonly BindableProperty PageContentProperty = BindableProperty.CreateAttached(
          propertyName: "PageContent",
          returnType: typeof(View),
          declaringType: typeof(FloatingActionButton),
          defaultValue: null);

    public View PageContent
    {
      get
      {
        return (View)GetValue(PageContentProperty);
      }
      set
      {
        SetValue(PageContentProperty, value);
      }
    }

    public static readonly BindableProperty IsMenuVisibleProperty = BindableProperty.CreateAttached(
          propertyName: "IsMenuVisible",
          returnType: typeof(bool),
          declaringType: typeof(FloatingActionButton),
          defaultValue: false);

    public bool IsMenuVisible
    {
      get
      {
        return (bool)GetValue(IsMenuVisibleProperty);
      }
      set
      {
        SetValue(IsMenuVisibleProperty, value);
      }
    }

    public static readonly BindableProperty MainButtonCommandProperty = BindableProperty.CreateAttached(
          propertyName: "MainButtonCommand",
          returnType: typeof(ICommand),
          declaringType: typeof(FloatingActionButton),
          defaultValue: null,
          propertyChanged: OnMainButtonActionChanged);

    public ICommand MainButtonCommand
    {
      get
      {
        return (ICommand)GetValue(MainButtonCommandProperty);
      }
      set
      {
        SetValue(MainButtonCommandProperty, value);
      }
    }

    public static readonly BindableProperty HasMenuProperty = BindableProperty.CreateAttached(
          propertyName: "HasMenu",
          returnType: typeof(bool),
          declaringType: typeof(FloatingActionButton),
          defaultValue: false,
          propertyChanged: OnMainButtonActionChanged);

    public bool HasMenu
    {
      get
      {
        return (bool)GetValue(HasMenuProperty);
      }
      set
      {
        SetValue(HasMenuProperty, value);
      }
    }

    static void OnButtonVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
      // selon la visibilité du bouton, on réalise une transition pour le mettre hors du visuel

      int translateTo = 1000;

      if ((bool)newValue)
      {
        translateTo = 0;
      }

      (bindable as FloatingActionButton).mainButton.TranslateTo(0, translateTo, 300, Easing.CubicInOut);
    }

    static void OnMainButtonActionChanged(BindableObject bindable, object oldValue, object newValue)
    {
      (bindable as FloatingActionButton).ChangeMainButtonAction();
    }

    async Task ChangeMenuVisibility()
    {
      // si on affiche le menu alors on le fait au début pour que les animations soient visibles
      // et si on le cache alors on le fera à la fin toujours pour que les animations soient visibles !

      Color startingColor = Color.Transparent;
      Color endingColor = Color.FromHex("#AFCCCCCC");
      uint duration = 300;
      int rotation = 360 * 2;
      bool menuIsShowing = true;
      int fadeTo = 1;
      Easing easingAnimation = Easing.CubicInOut;
      int translateTo = 0;

      if (!IsMenuVisible)
      {
        if (needToRebuildMenu)
        {
          BuildMenu();
          needToRebuildMenu = false;
        }

        if (Device.RuntimePlatform != Device.Android)
        {
          MenuContainer.Opacity = 0;
        }
        await MenuContainer.TranslateTo(0, 500, 1, Easing.Linear);

        IsMenuVisible = true;
      }
      else
      {
        fadeTo = 0;
        rotation = -1 * rotation;
        startingColor = endingColor;
        endingColor = Color.Transparent;
        duration = 200;
        translateTo = 500;

        menuIsShowing = false;
      }

      //TODO : bug sur Android a cause de l'opacité (https://bugzilla.xamarin.com/show_bug.cgi?id=51238)
      // supprimer la condition dès que le bug est corrigé
      if (Device.RuntimePlatform == Device.Android)
      {
        await Task.WhenAll(
          MenuContainer.TranslateTo(0, translateTo, 150, easingAnimation),
          mainButton.RotateTo(rotation, 300, easingAnimation),
          lockBox.ColorTo(startingColor, endingColor, c => lockBox.BackgroundColor = c, 300, easingAnimation)
        );
      }
      else
      {
        await Task.WhenAll(
          MenuContainer.TranslateTo(0, translateTo, 150, easingAnimation),
          MenuContainer.FadeTo(fadeTo, duration, easingAnimation),
          mainButton.RotateTo(rotation, 300, easingAnimation),
          lockBox.ColorTo(startingColor, endingColor, c => lockBox.BackgroundColor = c, 300, easingAnimation)
        );
      }

      if (IsMenuVisible && !menuIsShowing)
      {
        IsMenuVisible = false;
      }
    }

    public async Task ClickEffect()
    {
      int taskDelay = 50;

      if (Device.RuntimePlatform == Device.iOS)
      {
        taskDelay = 10;
      }

      mainButton.Opacity = 0.6;
      await mainButton.ScaleTo(0.9, 50, Easing.Linear);
      await Task.Delay(taskDelay);
      await mainButton.ScaleTo(1, 50, Easing.Linear);
      mainButton.Opacity = 1;
    }

    public ICommand MainButtonInternalCommand { get; set; }

    private void ChangeMainButtonAction()
    {
      mainButton.GestureRecognizers.Clear();

      // A cause du bug https://bugzilla.xamarin.com/show_bug.cgi?id=45014 sur iOS
      // on ne doit avoir qu'une seule commande sinon cela ne fonctionne pas...

      // si on a un menu à afficher
      if (HasMenu)
      {
        // on construit le menu
        BuildMenu();
        // et on dit que le bouton principal gère la visibilité du menu
        MainButtonInternalCommand = new Command(async () =>
        {
          await ChangeMenuVisibility();
        });
      }
      else
      {
        // on déconstruit le menu
        EmptyMenuContainer();
        // et on branche la commande de l'utilisateur sur le bouton principal
        MainButtonInternalCommand = MainButtonCommand;
      }

      mainButton.GestureRecognizers.Add(new TapGestureRecognizer
      {
        Command = new Command(async () =>
        {
          await ClickEffect();

          MainButtonInternalCommand.Execute(this);
        },
        () =>
        {
          return MainButtonInternalCommand.CanExecute(this);
        }),
        NumberOfTapsRequired = 1
      });
    }

    private void EmptyMenuContainer()
    {
      foreach (var item in MenuItems)
      {
        item.PropertyChanged -= FABMenuItemPropertyChanged;
      }

      MenuContainer.Children.Clear();
    }

    private ICommand AsyncFoldDownMenuCommand;

    public FloatingActionButton()
    {
      InitializeComponent();

      // commande qui sera passée entre autre aux boutons du menu pour permettre le repliage automatique dudit menu au clic
      AsyncFoldDownMenuCommand = new Command(async () =>
           {
             if (IsMenuVisible)
             {
               await ChangeMenuVisibility();
             }
           });

      // lorsque l'on clic en dehors du menu (ie. sur le boxview de lock) on referme le menu
      lockBox.GestureRecognizers.Add(new TapGestureRecognizer
      {
        Command = AsyncFoldDownMenuCommand,
        NumberOfTapsRequired = 1
      });

      // Aux changements des items du menu on indique qu'il va falloir reconstruire au prochain affichage
      // on ne le construit pas ici sinon on va le faire à chaque changement et ce ne sera pas optimisé / performant
      MenuItems.CollectionChanged += (sender, e) =>
      {
        needToRebuildMenu = true;
      };
    }

    private bool needToRebuildMenu = false;
  }
}

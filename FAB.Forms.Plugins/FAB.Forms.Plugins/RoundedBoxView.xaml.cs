using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace FAB.Forms.Plugins
{
  public partial class RoundedBoxView : Frame
  {
    #region #[CONSTANTES PUBLIQUES]##########################################################

    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.CreateAttached(
          propertyName: "ButtonSize",
          returnType: typeof(ButtonSize),
          declaringType: typeof(RoundedBoxView),
          defaultValue: ButtonSize.Default,
          propertyChanged: OnContentChanged);
    public static readonly BindableProperty MenuImageSourceProperty = BindableProperty.CreateAttached(
          propertyName: "MenuImageSource",
          returnType: typeof(string),
          declaringType: typeof(RoundedBoxView),
          defaultValue: null,
          propertyChanged: OnContentChanged);
    public static readonly BindableProperty CommandProperty = BindableProperty.CreateAttached(
          propertyName: "Command",
          returnType: typeof(ICommand),
          declaringType: typeof(RoundedBoxView),
          defaultValue: null,
          propertyChanging: OnCommandChanging,
          propertyChanged: OnContentChanged);

    #endregion

    #region #[CONSTRUCTEURS]#################################################################

    private bool _disableClickEffect = false;

    internal bool DisableClickEffect
    {
      get
      {
        return _disableClickEffect;
      }
      set
      {
        _disableClickEffect = value;
      }
    }

    private bool _ignoreDefaultLayoutOptions = false;

    internal bool IgnoreDefaultLayoutOptions
    {
      get
      {
        return _ignoreDefaultLayoutOptions;
      }
      set
      {
        _ignoreDefaultLayoutOptions = value;
      }
    }

    public RoundedBoxView()
    {
      InitializeComponent();

      Padding = 0;
      Margin = new Thickness(0, 5, 0, 5);

      BuildContent(this);
    }

    #endregion

    #region #[PROPRIÉTÉS]####################################################################

    public ButtonSize ButtonSize
    {
      get
      {
        return (ButtonSize)GetValue(ButtonSizeProperty);
      }
      set
      {
        SetValue(ButtonSizeProperty, value);
      }
    }

    public ICommand Command
    {
      get
      {
        return (ICommand)GetValue(CommandProperty);
      }
      set
      {
        SetValue(CommandProperty, value);
      }
    }

    public string MenuImageSource
    {
      get
      {
        return (string)GetValue(MenuImageSourceProperty);
      }
      set
      {
        SetValue(MenuImageSourceProperty, value);
      }
    }

    #endregion

    #region #[MÉTHODES PRIVÉES]##############################################################

    internal async Task ClickEffect()
    {
      int taskDelay = 50;

      if (Device.RuntimePlatform == Device.iOS)
      {
        taskDelay = 10;
      }

      this.Opacity = 0.6;
      await this.ScaleTo(0.9, 50, Easing.Linear);
      await Task.Delay(taskDelay);
      await this.ScaleTo(1, 50, Easing.Linear);
      this.Opacity = 1;
      await Task.Delay(taskDelay);
    }

    private void BuildContent(BindableObject bindable)
    {
      int imageSize = 20;
      int cornerRadius = 28;
      int widthAndHeightRequest = 56;

      if (ButtonSize == ButtonSize.Mini)
      {
        if(Device.RuntimePlatform == Device.Android)
        {
          imageSize = 14;
        }
        else
        {
          imageSize = 14;
        }

        cornerRadius = 20;
        widthAndHeightRequest = 40;
      }
      else
      {
        if (Device.RuntimePlatform == Device.Android)
        {
          imageSize = 25;
        }
      }

      Image menuImage = new Image();
      menuImage.Source = MenuImageSource;
      menuImage.WidthRequest = imageSize;
      menuImage.VerticalOptions = LayoutOptions.Center;
      menuImage.HorizontalOptions = LayoutOptions.Center;

      ContentView content = new ContentView();
      content.Padding = 0;
      content.Margin = 0;
      content.Content = menuImage;

      bindable.SetValue(RoundedBoxView.ContentProperty, content);
      bindable.SetValue(RoundedBoxView.CornerRadiusProperty, cornerRadius);
      bindable.SetValue(RoundedBoxView.HeightRequestProperty, widthAndHeightRequest);
      bindable.SetValue(RoundedBoxView.WidthRequestProperty, widthAndHeightRequest);

      if (!IgnoreDefaultLayoutOptions)
      {
        bindable.SetValue(RoundedBoxView.VerticalOptionsProperty, LayoutOptions.Center);
        bindable.SetValue(RoundedBoxView.HorizontalOptionsProperty, LayoutOptions.Center);
      }

      // A cause du bug https://bugzilla.xamarin.com/show_bug.cgi?id=45014 sur iOS
      // on ne doit avoir qu'une seule commande sinon cela ne fonctionne pas...

      if (Command != null)
      {
        originalColor = BackgroundColor;

        this.GestureRecognizers.Clear();

        // une première commande pour gérer l'effet de clic sur le bouton
        this.GestureRecognizers.Add(new TapGestureRecognizer
        {
          Command = new Command(async () =>
          {
            if (!DisableClickEffect)
            {
              await ClickEffect();
            }

            Command.Execute(this);
          }, () =>
          {
            return Command.CanExecute(this);

          }),
          NumberOfTapsRequired = 1
        });
      }
    }

    private Color originalColor;

    private void OnCommandExecuteChanged(object sender, EventArgs args)
    {
      if (Command.CanExecute(this))
      {
        BackgroundColor = originalColor;
      }
      else {
        this.BackgroundColor = Color.FromHex("FFD3D3D3");
      }
    }

    static void OnCommandChanging(BindableObject bindable, object oldValue, object newValue)
    {
      if ((oldValue as ICommand) != null)
      {
        (oldValue as ICommand).CanExecuteChanged -= (bindable as RoundedBoxView).OnCommandExecuteChanged;
      }

      if ((newValue as ICommand) != null)
      {
        (newValue as ICommand).CanExecuteChanged += (bindable as RoundedBoxView).OnCommandExecuteChanged;
      }
    }

    /// <summary>
    /// Au changement de valeur, on change l'image et le contenu complet
    /// </summary>
    /// <param name="bindable">Bindable.</param>
    /// <param name="oldValue">Old value.</param>
    /// <param name="newValue">New value.</param>
    static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
      if (bindable is RoundedBoxView)
      {
        (bindable as RoundedBoxView).BuildContent(bindable);
      }
    }

    public string ItemId { get; set; }

    #endregion
  }
}
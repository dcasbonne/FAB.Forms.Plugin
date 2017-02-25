using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace FAB.Forms.Plugins
{
  public partial class RoundedTitledBoxView : ContentView
  {
    #region #[CONSTANTES PUBLIQUES]##########################################################

    public static readonly BindableProperty ButtonBackgroundColorProperty = BindableProperty.CreateAttached(
          propertyName: "ButtonBackgroundColor",
          returnType: typeof(Color),
          declaringType: typeof(RoundedTitledBoxView),
          defaultValue: Color.Aqua);
    public static readonly BindableProperty ButtonSizeProperty = BindableProperty.CreateAttached(
          propertyName: "ButtonSize",
          returnType: typeof(ButtonSize),
          declaringType: typeof(RoundedTitledBoxView),
          defaultValue: ButtonSize.Default,
          propertyChanged: OnContentChanged);
    public static readonly BindableProperty CommandProperty = BindableProperty.CreateAttached(
          propertyName: "Command",
          returnType: typeof(ICommand),
          declaringType: typeof(RoundedTitledBoxView),
          defaultValue: null,
          propertyChanged: OnCommandChanged);
    public static readonly BindableProperty FrameMarginProperty = BindableProperty.CreateAttached(
          propertyName: "FrameMargin",
          returnType: typeof(Thickness),
          declaringType: typeof(RoundedTitledBoxView),
          defaultValue: new Thickness(0, 0, 7, 0),
          propertyChanged: OnContentChanged);
    public static readonly BindableProperty MenuImageSourceProperty = BindableProperty.CreateAttached(
          propertyName: "MenuImageSource",
          returnType: typeof(string),
          declaringType: typeof(RoundedTitledBoxView),
          defaultValue: null,
          propertyChanged: OnContentChanged);
    public static readonly BindableProperty TitleProperty = BindableProperty.CreateAttached(
          propertyName: "Title",
          returnType: typeof(string),
          declaringType: typeof(RoundedTitledBoxView),
          defaultValue: null);

    #endregion

    #region #[VARIABLES PRIVÉES]#############################################################

    private Guid _itemId;

    #endregion

    #region #[CONSTRUCTEURS]#################################################################

    public ICommand SpecialParentCommand { get; private set; }

    public RoundedTitledBoxView(Guid itemId, ICommand parentCommand)
    {
      InitializeComponent();

      _itemId = itemId;
      SpecialParentCommand = parentCommand;
    }

    public RoundedTitledBoxView()
      : this(Guid.NewGuid(), null)
    {
    }

    private bool _showTitle = true;

    public bool ShowTitle
    {
      get
      {
        return _showTitle;
      }
      set
      {
        _showTitle = value;
      }
    }

    #endregion

    #region #[PROPRIÉTÉS]####################################################################

    public Color ButtonBackgroundColor
    {
      get
      {
        return (Color)GetValue(ButtonBackgroundColorProperty);
      }
      set
      {
        SetValue(ButtonBackgroundColorProperty, value);
      }
    }

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

    public Thickness FrameMargin
    {
      get
      {
        return (Thickness)GetValue(FrameMarginProperty);
      }
      set
      {
        SetValue(FrameMarginProperty, value);
      }
    }

    public Guid ItemId
    {
      get
      {
        return _itemId;
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

    public string Title
    {
      get
      {
        return (string)GetValue(TitleProperty);
      }
      set
      {
        SetValue(TitleProperty, value);
      }
    }

    #endregion

    #region #[PROPRIÉTÉS : Commandes]########################################################

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

    #endregion

    #region #[MÉTHODES PRIVÉES]##############################################################

    public async Task ClickEffect()
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

    private void AdjustContent(BindableObject bindable)
    {
      if (ButtonSize == ButtonSize.Mini)
      {
        bindable.SetValue(RoundedTitledBoxView.FrameMarginProperty, new Thickness(0, 0, 15, 0));
      }

      // si on ne veut pas afficher le titre ou qu'il est null on cache le Frame qui le contient
      labelFrame.IsVisible = (ShowTitle && !string.IsNullOrEmpty(Title));
    }
    public ICommand InternalCommand { get; private set; }

    private void ChangeCommand()
    {
      labelFrame.GestureRecognizers.Clear();

      // A cause du bug https://bugzilla.xamarin.com/show_bug.cgi?id=45014 sur iOS
      // on ne doit avoir qu'une seule commande sinon cela ne fonctionne pas...

      // Cette commande sera utilisée pour cliquer sur le bouton ou sur le frame du label directement
      InternalCommand = new Command(async () =>
        {
          await ClickEffect();

          if (SpecialParentCommand != null)
          {
            SpecialParentCommand.Execute(this);
          }

          if (Command != null)
          {
            Command.Execute(this);
            Command.CanExecuteChanged += OnCommandExecuteChanged;
          }
        }, () =>
        {
          return ((SpecialParentCommand == null) || (SpecialParentCommand != null && SpecialParentCommand.CanExecute(this)))
            && ((Command == null) || (Command != null && Command.CanExecute(this)));
        });

      labelFrame.GestureRecognizers.Add(new TapGestureRecognizer
      {
        Command = InternalCommand,
        NumberOfTapsRequired = 1
      });
    }

    private Color originalColor = Color.Blue;

    private void OnCommandExecuteChanged(object sender, EventArgs args)
    {
      if (Command.CanExecute(this))
      {
        ButtonBackgroundColor = originalColor;
        labelFrame.BackgroundColor = originalColor;
        labelTitle.FontAttributes = FontAttributes.None;

      }
      else {
        ButtonBackgroundColor = Color.FromHex("FFD3D3D3");
        labelFrame.BackgroundColor = Color.FromHex("FFD3D3D3");
        labelTitle.FontAttributes = FontAttributes.Italic;
      }
    }

    static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
      if (bindable is RoundedTitledBoxView)
      {
        (bindable as RoundedTitledBoxView).ChangeCommand();
      }
    }

    static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
      if (bindable is RoundedTitledBoxView)
      {
        (bindable as RoundedTitledBoxView).AdjustContent(bindable);
      }
    }

    #endregion
  }
}
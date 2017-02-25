using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace FAB.Forms.Plugins
{
  public class FloatingActionButtonMenuItem : View
  {
    #region #[CONSTANTES PUBLIQUES]##########################################################

    public static readonly BindableProperty ButtonBackgroundColorProperty = BindableProperty.CreateAttached(
          propertyName: "ButtonBackgroundColor",
          returnType: typeof(Color),
          declaringType: typeof(FloatingActionButtonMenuItem),
          defaultValue: Color.Aqua);
    public static readonly BindableProperty ButtonCommandProperty = BindableProperty.CreateAttached(
          propertyName: "ButtonCommand",
          returnType: typeof(ICommand),
          declaringType: typeof(FloatingActionButtonMenuItem),
          defaultValue: null);
    public static readonly BindableProperty MenuImageSourceProperty = BindableProperty.CreateAttached(
          propertyName: "MenuImageSource",
          returnType: typeof(string),
          declaringType: typeof(FloatingActionButtonMenuItem),
          defaultValue: null);
    public static readonly BindableProperty TitleProperty = BindableProperty.CreateAttached(
          propertyName: "Title",
          returnType: typeof(string),
          declaringType: typeof(FloatingActionButtonMenuItem),
          defaultValue: null);

    #endregion

    #region #[VARIABLES PRIVÉES]#############################################################

    private Guid _itemId = Guid.NewGuid();

    #endregion


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

    /// <summary>
    /// Identifiant permettant de retrouver le bouton dans la liste
    /// </summary>
    /// <value>The item identifier.</value>
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

    public ICommand ButtonCommand
    {
      get
      {
        return (ICommand)GetValue(ButtonCommandProperty);
      }
      set
      {
        SetValue(ButtonCommandProperty, value);
      }
    }

    #endregion
  }
}

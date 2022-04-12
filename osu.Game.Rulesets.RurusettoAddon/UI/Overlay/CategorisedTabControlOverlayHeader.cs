using osu.Framework.Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay;

public class CategorisedTabItem<Tcategory, Ttab> {
    public Tcategory Category { get; init; }
    public Ttab Tab { get; init; }
}

public abstract class CategorisedTabControlOverlayHeader<T, Tcategory, Ttab> : OverlayHeader, IHasCurrentValue<T> where T : CategorisedTabItem<Tcategory, Ttab> {
    protected OsuTabControl<T> TabControl;

    private readonly Box controlBackground;
    private readonly Container tabControlContainer;
    private readonly BindableWithCurrent<T> current = new();
    public Bindable<T> Current {
        get => current.Current;
        set => current.Current = value;
    }

    protected OsuTabControl<Tcategory> CategoryControl;

    private readonly Box categoryControlBackground;
    private readonly Container categoryControlContainer;
    private readonly BindableWithCurrent<Tcategory> currentCategory = new();
    public Bindable<Tcategory> CurrentCategory {
        get => currentCategory.Current;
        set => currentCategory.Current = value;
    }

    protected new float ContentSidePadding {
        get => base.ContentSidePadding;
        set {
            base.ContentSidePadding = value;
            tabControlContainer.Padding = new MarginPadding { Horizontal = value };
        }
    }

    protected CategorisedTabControlOverlayHeader () {
        HeaderInfo.Add( new FillFlowContainer {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                    new Container {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Depth = -1,
                        Children = new Drawable[] {
                            categoryControlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            categoryControlContainer = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Horizontal = ContentSidePadding },
                                Child = CategoryControl = CreateCategoryControl().With(control =>
                                {
                                    control.Current = CurrentCategory;
                                })
                            }
                        }
                    },
                    new Container {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[] {
                            controlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            tabControlContainer = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Horizontal = ContentSidePadding },
                                Child = TabControl = CreateTabControl().With(control =>
                                {
                                    control.Current = Current;
                                })
                            }
                        }
                    }
            }
        } );
    }

    [BackgroundDependencyLoader]
    private void load ( OverlayColourProvider colourProvider ) {
        controlBackground.Colour = colourProvider.Dark3;
        categoryControlBackground.Colour = colourProvider.Dark4;
    }

    protected virtual OsuTabControl<T> CreateTabControl () => new OverlayHeaderTabControl<T>();
    protected virtual OsuTabControl<Tcategory> CreateCategoryControl () => new OverlayHeaderTabControl<Tcategory>();

    public class OverlayHeaderTabControl<T> : OverlayTabControl<T> {
        private const float bar_height = 1;

        public OverlayHeaderTabControl () {
            RelativeSizeAxes = Axes.None;
            AutoSizeAxes = Axes.X;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            Height = 47;
            BarHeight = bar_height;
        }

        protected override TabItem<T> CreateTabItem ( T value ) => new OverlayHeaderTabItem( value );

        protected override TabFillFlowContainer CreateTabFlow () => new TabFillFlowContainer {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Direction = FillDirection.Horizontal,
        };

        private class OverlayHeaderTabItem : OverlayTabItem {
            public OverlayHeaderTabItem ( T value )
                : base( value ) {
                if ( !( Value is Enum enumValue ) )
                    Text.Text = Value.ToString().ToLower();
                else {
                    var localisableDescription = enumValue.GetLocalisableDescription();
                    string nonLocalisableDescription = enumValue.GetDescription();

                    // If localisable == non-localisable, then we must have a basic string, so .ToLower() is used.
                    Text.Text = localisableDescription.Equals( nonLocalisableDescription )
                        ? nonLocalisableDescription.ToLower()
                        : localisableDescription;
                }

                Text.Font = OsuFont.GetFont( size: 14 );
                Text.Margin = new MarginPadding { Vertical = 16.5f }; // 15px padding + 1.5px line-height difference compensation
                Bar.Margin = new MarginPadding { Bottom = bar_height };
            }
        }
    }
}
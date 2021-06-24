using DotX.PropertySystem;
using DotX.Widgets.Templates;

namespace DotX.Widgets
{
    public class ContentPresenter : Control
    {
        static ContentPresenter()
        {
            CompositeObjectProperty.OverrideProperty<Template, ContentPresenter>(TemplateProperty,
                                                                                 new EmptyTemplate(),
                                                                                 changeValueFunc: (cp, ov, nv) => cp.OnTemplateChanged());
        }

        public static readonly CompositeObjectProperty SourcePropertyProperty =
            CompositeObjectProperty.RegisterProperty<CompositeObjectProperty, ContentPresenter>(nameof(SourceProperty),
                                                                                                PropertyOptions.Inherits,
                                                                                                Control.ContentProperty,
                                                                                                changeValueFunc: OnSourcePropertyPropertyChanged);

        private static void OnSourcePropertyPropertyChanged(ContentPresenter presenter,
                                                            CompositeObjectProperty oldValue,
                                                            CompositeObjectProperty newValue)
        {
            if(presenter._changingSource)
                return;

            presenter._changingSource = true;

            presenter.ContentSourceName = null;
            presenter.InvalidateSource();

            presenter._changingSource = false;
        }

        public CompositeObjectProperty SourceProperty
        {
            get => GetValue<CompositeObjectProperty>(SourcePropertyProperty);
            set => SetValue(SourcePropertyProperty, value);
        }

        public static readonly CompositeObjectProperty ContentSourceNameProperty =
            CompositeObjectProperty.RegisterProperty<string, ContentPresenter>(nameof(ContentSourceName),
                                                                               PropertyOptions.Inherits,
                                                                               string.Empty,
                                                                               changeValueFunc: OnContentSourceNamePropertyChanged);

        private static void OnContentSourceNamePropertyChanged(ContentPresenter presenter,
                                                               string oldValue,
                                                               string newValue)
        {
            if(presenter._changingSource)
                return;

            presenter._changingSource = true;

            presenter.SourceProperty = null;
            presenter.InvalidateSource();

            presenter._changingSource = false;
        }

        public string ContentSourceName
        {
            get => GetValue<string>(ContentSourceNameProperty);
            set => SetValue(ContentSourceNameProperty, value);
        }

        private bool _changingSource = false;

        public CompositeObject TemplatedParent
        {
            get;
            set;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            //TODO: use binding
        }

        private void InvalidateSource()
        {
            if(SourceProperty is not null)
            {
                Content = TemplatedParent.GetValue<Visual>(SourceProperty);
            }
            else if(ContentSourceName is not null)
            {
                Content = (Visual)TemplatedParent.GetType()
                                                 .GetProperty(ContentSourceName)
                                                 .GetValue(TemplatedParent);
            }
            else
            {
                Content = default;
            }
        }
    }
}
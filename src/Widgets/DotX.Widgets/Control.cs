using System;
using Cairo;
using DotX.Data;
using DotX.Extensions;
using DotX.Attributes;
using DotX.PropertySystem;
using System.Linq;
using DotX.Widgets.Templates;
using DotX.Interfaces;

namespace DotX.Widgets
{
    [ContentMember(nameof(Control.Content))]
    public class Control : Widget
    {
        static Control()
        {}

        public static readonly CompositeObjectProperty ContentProperty = 
            CompositeObjectProperty.RegisterProperty<Visual, Control>(nameof(Content),
                                                                      PropertyOptions.Inherits |
                                                                      PropertyOptions.AffectsMeaure |
                                                                      PropertyOptions.AffectsArrange |
                                                                      PropertyOptions.AffectsRender,
                                                                      changeValueFunc: OnContentPropertyChanged);

        private static void OnContentPropertyChanged(Control control, Visual oldValue, Visual newValue)
        {
            control.OnContentChanged(oldValue, newValue);
        }

        public static readonly CompositeObjectProperty TemplateProperty =
            CompositeObjectProperty.RegisterProperty<Template, Control>(nameof(Template),
                                                                        PropertyOptions.Inherits |
                                                                        PropertyOptions.AffectsMeaure |
                                                                        PropertyOptions.AffectsRender,
                                                                        new EmptyTemplate(),
                                                                        changeValueFunc: OnTemplatePropertyChanged);

        private static void OnTemplatePropertyChanged(Control control, 
                                                      Template oldValue,
                                                      Template newValue)
        {
            control.OnTemplateChanged();
        }

        public Visual Content
        {
            get => GetValue<Visual>(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public Template Template
        {
            get => GetValue<Template>(TemplateProperty);
            set => SetValue(TemplateProperty, value);
        }

        internal Visual Child
        {
            get => VisualChildren.FirstOrDefault();
            set
            {
                if(VisualChildren.Any())
                    throw new Exception();

                VisualChildren.Add(value);
            }
        }

        protected override void OnRender(Context context)
        {
            context.Save();
            base.OnRender(context);
            context.Restore();

            if (Child is null)
                return;

            Child.Render(context);
        }

        private void OnContentChanged(Visual oldValue, Visual newValue)
        {
            if(oldValue is Widget oldWidget)
                oldWidget.LogicalParent = null;

            if(newValue?.VisualParent is not null)
                throw new Exception("Already has a parent");

            if(newValue is Widget newWidget)
                newWidget.LogicalParent = this;
        }

        protected override Size MeasureCore(Size size)
        {
            if(Child is null)
                return new (0, 0);

            var offset = Padding;
            if(Child is Widget w)
            {
                offset = new (offset.Left + w.Margin.Left,
                              offset.Top + w.Margin.Top,
                              offset.Right + w.Margin.Right,
                              offset.Bottom + w.Margin.Bottom);
            }

            Size adjustedSize = size.Subtract(offset);

            Child.Measure(adjustedSize);

            return base.MeasureCore(Child.DesiredSize.Add(offset));
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            if(!IsVisible || Child is null)
                return new Rectangle(size.X, size.Y, 0, 0);

            Margin margin = Padding;
            if(Child is Widget w)
            {
                margin = new (margin.Left + w.Margin.Left,
                              margin.Top + w.Margin.Top,
                              margin.Right + w.Margin.Right,
                              margin.Bottom + w.Margin.Bottom);
            }

            Child.Arrange(size.Subtract(margin));

            if(Stretch == StretchBehavior.Stretch)
                return size;
            else
                return Child.RenderSize.Add(margin);
        }

        protected override void ApplyStylesForChildren()
        {
            (Child as Widget)?.ApplyStyles();
        }

        public override void HitTest(HitTestResult result)
        {
            Child?.HitTest(result);

            base.HitTest(result);
        }

        protected override void OnInitialize()
        {
            if(Template is not null)
                Template.ApplyTo(this);
            else if(Child is IInitializable initializableChild)
                initializableChild.Initialize();
            else if(Content is IInitializable initializableContent)
                initializableContent.Initialize();
        }

        protected virtual void OnTemplateChanged()
        {
        }
    }
}
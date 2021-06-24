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

        [ContentProperty]
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

        public override void Render(Context context)
        {
            base.Render(context);

            if (!IsVisible || Child is null)
                return;

            Child.Render(context);
        }

        private void OnContentChanged(Visual oldValue, Visual newValue)
        {
            if(oldValue is Widget oldWidget)
                oldWidget.LogicalParent = null;

            if(newValue?.VisualParent is not null)
                throw new Exception("Already have a parent");

            if(newValue is Widget newWidget)
                newWidget.LogicalParent = this;
        }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            if(!IsVisible || Child is null)
                return new Rectangle(size.X, size.Y, 0, 0);

            Rectangle adjustedSize = size.Subtract(Padding);
            Widget w = default;
            if(Child is Widget)
            {
                w = (Widget)Child;
                adjustedSize = adjustedSize.Subtract(w.Margin);
            }

            Child.Measure(adjustedSize);

            Rectangle desiredSize = Child.DesiredSize.Add(Padding);
            if(w is not null)
                desiredSize = desiredSize.Add(w.Margin);

            return desiredSize;
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            if(!IsVisible || Child is null)
                return new Rectangle(size.X, size.Y, 0, 0);

            Rectangle adjustedSize = size.Subtract(Padding);
            Widget w = default;
            if(Child is Widget)
            {
                w = (Widget)Child;
                adjustedSize = adjustedSize.Subtract(w.Margin);
            }

            Child.Arrange(adjustedSize);

            Rectangle renderSize = Child.RenderSize.Add(Padding);
            if(w is not null)
                renderSize = renderSize.Add(w.Margin);
            
            return renderSize;
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
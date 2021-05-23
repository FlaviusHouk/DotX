using System.Collections.Generic;
using System.Linq;
using DotX.PropertySystem;

namespace DotX.Styling
{
    public class Style
    {
        public Selector Selector { get; set; }

        public ICollection<Setter> Setters { get; }
            = new HashSet<Setter>();

        public bool TryAttach(CompositeObject obj)
        {
            bool attached = false;

            var props = PropertyManager.Instance.GetProperties(obj.GetType());
            
            foreach(var setter in Setters.Where(s => props.Any(p => p.PropName == s.Property)))
            {
                setter.SetValue(obj);
                
                attached = true;
            }

            return attached;
        }

        public void Detach(CompositeObject obj)
        {
            foreach(var setter in Setters)
                setter.UnsetValue(obj);
        }
    }
}
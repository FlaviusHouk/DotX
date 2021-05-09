namespace DotX
{
    public class CompositeObject
    {
        public CompositeObject()
        {
            var props = PropertyManager.Instance.GetProperties(GetType());
            ValueStorage.Storage.Init(this, props);
        }

        public T GetValue<T>(CompositeObjectProperty prop)
        {
            if(!CanSet(prop))
                throw new System.Exception();

            prop = PropertyManager.Instance.GetVirtualProperty(GetType(), prop);
                
            return ValueStorage.Storage.GetValue<T>(this, prop);
        }

        public void SetValue<T>(CompositeObjectProperty prop, T value)
        {
            if(!CanSet(prop))
                throw new System.Exception();

            prop = PropertyManager.Instance.GetVirtualProperty(GetType(), prop);

            ValueStorage.Storage.SetValue<T>(this, prop, value);
        }

        public bool IsPropertySet(CompositeObjectProperty prop)
        {
            return ValueStorage.Storage.IsSet(this, prop);
        }

        private bool CanSet(CompositeObjectProperty prop)
        {
            return PropertyManager.Instance.IsPropertyAvailable(GetType(), prop);
        }
    }
}
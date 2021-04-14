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
            if(!PropertyManager.Instance.IsPropertyAvailable(GetType(), prop))
                throw new System.Exception();
                
            return ValueStorage.Storage.GetValue<T>(this, prop);
        }

        public void SetValue<T>(CompositeObjectProperty prop, T value)
        {
            if(!PropertyManager.Instance.IsPropertyAvailable(GetType(), prop))
                throw new System.Exception();

            ValueStorage.Storage.SetValue<T>(this, prop, value);
        }
    }
}
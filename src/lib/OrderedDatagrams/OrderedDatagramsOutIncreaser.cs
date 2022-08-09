namespace Piot.Surge.OrderedDatagrams
{
    public class OrderedDatagramsOutIncrease
    {
        private OrderedDatagramsOut value;

        public OrderedDatagramsOutIncrease()
        {
        }

        public OrderedDatagramsOutIncrease(OrderedDatagramsOut startValue)
        {
            value = startValue;
        }

        public OrderedDatagramsOut Value => value;

        public void Increase()
        {
            value = value.Next();
        }
    }
}
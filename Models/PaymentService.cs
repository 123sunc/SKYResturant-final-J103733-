namespace SKYResturant.Models
{
    public class PaymentService
    {
         
        public bool ProcessPayment(decimal amount, string cardNumber, int expiryMonth, int expiryYear, string cvv)
        {
            // Simulate processing payment
            // Here, you can check the card number, expiry, and cvv to simulate success or failure
            return true;
        }
    }
}

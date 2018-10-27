using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatelessTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Test2();
        }

        public static void Test2()
        {
            var orderItem = new OrderItemEntity();
            orderItem.ProductId = 1;
            orderItem.SupplierId = 1;
            orderItem.SetStatus(OrderItemStatus.Enquired);

            var stateMachine = new StateMachine<OrderItemStatus, EventType>(() => orderItem.Status, status => orderItem.SetStatus(status));

            stateMachine.Configure(OrderItemStatus.Enquired)
               .Permit(EventType.SupplierConfirm, OrderItemStatus.SupplierConfirmed)
               .Permit(EventType.Cancel, OrderItemStatus.Cancelled);

            stateMachine.Configure(OrderItemStatus.SupplierConfirmed)
                .OnEntry(() => { SupplierConfirm(orderItem, orderItem.Status, stateMachine.State); })
                .OnExit(() => { SupplierConfirmed(orderItem, stateMachine.State); })
                .Permit(EventType.Confirm, OrderItemStatus.Confirmed)
                .Permit(EventType.Cancel, OrderItemStatus.Cancelled);

            Console.WriteLine(stateMachine.State);
            Console.WriteLine(orderItem.Status);

            stateMachine.Fire(EventType.SupplierConfirm);
            Console.WriteLine(stateMachine.State);
            Console.WriteLine(orderItem.Status);

            var validate1 = stateMachine.IsInState(OrderItemStatus.SupplierConfirmed);

            stateMachine.Fire(EventType.Cancel);
            Console.WriteLine(stateMachine.State);
            Console.WriteLine(orderItem.Status);

            string graph = UmlDotGraph.Format(stateMachine.GetInfo());

            Console.ReadLine();
        }

        public static void Test1()
        {
            var stateMachine = new StateMachine<OrderItemStatus, EventType>(OrderItemStatus.Enquired);

            stateMachine.Configure(OrderItemStatus.Enquired)
                .Permit(EventType.SupplierConfirm, OrderItemStatus.SupplierConfirmed)
                .Permit(EventType.Cancel, OrderItemStatus.Cancelled);

            stateMachine.Configure(OrderItemStatus.SupplierConfirmed)
                .OnEntry(() => { Console.WriteLine("Supplier Confirmed Entry"); })
                .OnExit(() => { Console.WriteLine("Supplier Confirmed Exit"); })
                .Permit(EventType.Confirm, OrderItemStatus.Confirmed)
                .Permit(EventType.Cancel, OrderItemStatus.Cancelled);

            Console.WriteLine(stateMachine.State);

            stateMachine.Fire(EventType.SupplierConfirm);
            Console.WriteLine(stateMachine.State);

            var validate1 = stateMachine.IsInState(OrderItemStatus.SupplierConfirmed);

            stateMachine.Fire(EventType.Cancel);
            Console.WriteLine(stateMachine.State);

            string graph = UmlDotGraph.Format(stateMachine.GetInfo());

            Console.ReadLine();
        }

        private static void SupplierConfirm(OrderItemEntity item, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
            Console.WriteLine($"Supplier Confirmed Entry. Old Status {oldStatus},New Status {newStatus}");
        }

        private static void SupplierConfirmed(OrderItemEntity item, OrderItemStatus status)
        {
            Console.WriteLine($"Supplier Confirmed Exit. Status {status}");
        }
    }

    public class OrderItemEntity
    {
        public int ProductId { get; set; }

        public int SupplierId { get; set; }

        public OrderItemStatus Status { get; private set; }

        public void SetStatus(OrderItemStatus status)
        {
            this.Status = status;
        }
    }

    public enum OrderItemStatus
    {
        Enquired = 1,
        SupplierConfirmed = 2,
        Confirmed = 4,
        Payment = 8,
        Completed = 16,
        ToTravel = 32,
        Travelling = 64,
        Travelled = 128,
        Archived = 256,
        Cancelled = 1024
    }

    public enum EventType
    {
        SupplierConfirm = 0,
        Confirm = 5,
        Payment = 10,
        Complete = 15,
        Archive = 20,
        Cancel = 100
    }
}

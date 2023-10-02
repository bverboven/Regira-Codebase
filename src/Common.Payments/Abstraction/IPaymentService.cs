﻿namespace Regira.Payments.Abstraction;

public interface IPaymentService
{
    Task<IPayment?> Details(string paymentId);
    Task Save(IPayment item);
}
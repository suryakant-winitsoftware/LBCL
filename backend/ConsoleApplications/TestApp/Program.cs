using Winit.Shared.CommonUtilities.Common;
string opening_balance = "-2 NO.";
int qty = CommonFunctions.ExtractNumericQuantity(opening_balance);
Console.WriteLine($"Extracted Qty: {qty}");

// See https://aka.ms/new-console-template for more information
string ISBN_13 = "9788196712488";
string ISBN_9 = ISBN_13.Substring(3, 9);
int TotalSum_ISBN9 = 0;
int multiplier = 10;
foreach (char ch in ISBN_9.ToCharArray())
{
    int sum = 0;
    sum = Convert.ToInt32(ch.ToString()) * multiplier;
    TotalSum_ISBN9 = TotalSum_ISBN9 + sum;
    multiplier--;
}
int remainder = TotalSum_ISBN9 % 11;
int check_digit = 11 - remainder;
string check_value = "";

if (check_digit == 11)
{
    check_value = "0";
}
else if (check_digit == 10)
{
    check_value = "X";
}
else
{
    check_value = check_digit.ToString();
}
string ISBN_10 = ISBN_9 + check_value;
Console.WriteLine(ISBN_10);
Console.ReadKey();

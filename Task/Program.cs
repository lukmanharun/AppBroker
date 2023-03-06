//var list = new List<int>();
//var isValid = true;
//int i = 0;
//while (isValid)
//{
//    var input = Console.ReadLine();
//    list.Add(Convert.ToInt16(input));
//    i++;
//    if(i == 3) isValid = false;
//}
//int maxIndex = list.IndexOf(list.Max());
//var str = maxIndex == 0 ? "1st" : maxIndex == 1 ? "2nd" : "3rd";
//Console.WriteLine($"The {str} Number is the greatest among three");

int input = Convert.ToInt16(Console.ReadLine());
int sum = 0;
int hasil;
while(input>0)
{
    hasil = input % 10;
    sum = sum + hasil;
    input = input / 10;
}
Console.Write($"Sum of the digits of the said integer: {sum}");
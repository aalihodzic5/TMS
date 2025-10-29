using System;
using System.ComponentModel.DataAnnotations;

public class FutureOrTodayDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime dateValue)
        {
            return dateValue.Date >= DateTime.Now.Date;
        }
        return false;
    }
}

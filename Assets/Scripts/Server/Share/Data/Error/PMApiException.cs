using System;

public class PMApiException : Exception
{
    public PMErrorCode errorCode { get; set; }
    public string message { get; set; }
}

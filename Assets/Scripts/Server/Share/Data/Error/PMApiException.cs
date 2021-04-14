using System;

public class PMApiException : Exception
{
    public PMErrorCode erroCode { get; set; }
    public string message { get; set; }
}

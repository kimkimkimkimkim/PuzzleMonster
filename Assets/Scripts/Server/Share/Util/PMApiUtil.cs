using System;
using System.Collections.Generic;

// APIUtil
public static class PMApiUtil{
    public static void ErrorIf(bool condition, PMErrorCode errorCode, string message){
        if(condition) throw new PMApiException(){ errorCode = errorCode, message = message };
    }

    // valueDict => Key:変数名, Value:値
    public static void ErrorIf(bool condition, PMErrorCode errorCode, string message, Dictionary<string, object> valueDict){
        var newMessage = message;
        newMessage += " => [";
        foreach(var kvp in valueDict){
            newMessage += $"{kvp.Key}:{kvp.Key}, ";
        }
        newMessage += "]";
        ErrorIf(condition, errorCode, newMessage);
    }
}
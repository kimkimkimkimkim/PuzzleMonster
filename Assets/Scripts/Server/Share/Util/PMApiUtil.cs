// APIUtil
public static class PMApiUtil{
    public static void ErrorIf(bool condition, PMErrorCode errorCode, string message){
        if(condition) throw new PMApiException(){ errorCode = errorCode, message = message };
    }
}
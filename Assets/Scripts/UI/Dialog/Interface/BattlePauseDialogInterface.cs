public class BattlePauseDialogRequest
{
}

public class BattlePauseDialogResponse
{
    public BattlePauseDialogResponseType responseType { get; set; }
}

public enum BattlePauseDialogResponseType
{
    None,
    Close,
    Interruption,
}
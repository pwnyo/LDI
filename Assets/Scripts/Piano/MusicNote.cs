
[System.Serializable]
/// <summary>
/// Represents a music note
/// </summary>
public class MusicNote
{
    public MusicKey noteKey;

    public void PitchUp()
    {
        noteKey = GetHalfToneUp(noteKey);
    }
    public void PitchDown()
    {
        noteKey = GetHalfToneDown(noteKey);
    }
    public MusicKey GetHalfToneUp(MusicKey key)
    {
        switch (key)
        {
            case (MusicKey.C):
            case (MusicKey.D):
            case (MusicKey.F):
            case (MusicKey.G):
            case (MusicKey.A):
                return key + 1;
            default:
                return key;
        }
    }
    public MusicKey GetHalfToneDown(MusicKey key)
    {
        switch (key)
        {
            case (MusicKey.D):
            case (MusicKey.E):
            case (MusicKey.G):
            case (MusicKey.A):
            case (MusicKey.B):
                return key - 1;
            default:
                return key;
        }
    }
}

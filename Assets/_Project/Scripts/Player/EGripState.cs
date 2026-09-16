namespace Branded.Player
{
    // Which hands are on which weapon. The sword is held in both, so the crossbow can only be
    // aimed once the left hand has let go: the two transitions are states of their own, because
    // each one is an animation clip the arms have to actually play.
    public enum EGripState
    {
        Sword,
        Drawing,
        Crossbow,
        Stowing
    }
}

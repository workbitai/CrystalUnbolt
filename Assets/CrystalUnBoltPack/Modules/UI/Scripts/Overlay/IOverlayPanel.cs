namespace CrystalUnbolt
{
    public interface IOverlayPanel
    {
        public void Init();
        public void Clear();

        public void Show(float duration, GameCallback onCompleted);
        public void Hide(float duration, GameCallback onCompleted);

        public void SetState(bool state);
        public void SetLoadingState(bool state);
    }
}

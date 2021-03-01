using UniRx;

namespace GameBase
{
    public class SampleGameBaseManager : SingletonMonoBehaviour<SampleGameBaseManager>
    {
        private void Start()
        {
            SampleGameBaseWindowFactory.Create(new SampleGameBaseWindowRequest()).Subscribe();
        }
    }
}

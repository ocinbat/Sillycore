using System;

namespace Sillycore
{
    public interface ISillycoreAppBuilder
    {
        void BeforeBuild(Action action);

        void Build();

        void AfterBuild(Action action);
    }
}
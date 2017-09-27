using System;

namespace Sillycore
{
    public interface ISillycoreAppBuilder
    {
        void BeforeBuild(Action action);

        SillycoreApp Build();

        void AfterBuild(Action action);
    }
}
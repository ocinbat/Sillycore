using System;

namespace Sillycore
{
    public class SillycoreAppModule : ISillycoreAppBuilder
    {
        private readonly SillycoreAppBuilder _builder;

        protected SillycoreAppModule(SillycoreAppBuilder builder1)
        {
            _builder = builder1;
        }

        public void BeforeBuild(Action action)
        {
            _builder.BeforeBuild(action);
        }

        public void Build()
        {
            _builder.Build();
        }

        public void AfterBuild(Action action)
        {
            _builder.AfterBuild(action);
        }

        public SillycoreAppBuilder Then()
        {
            return _builder;
        }
    }
}
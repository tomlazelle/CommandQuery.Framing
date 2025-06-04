using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace CommandTests.Configuration;

public abstract class Subject<TClassUnderTest>
    where TClassUnderTest : class
{
    protected IFixture _fixture;

    private TClassUnderTest _sut;

    protected Subject()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization
        {
            GenerateDelegates = true
        });
    }

    protected TClassUnderTest Sut
    {
        get { return _sut ?? (_sut = new Lazy<TClassUnderTest>(() => _fixture.Create<TClassUnderTest>()).Value); }
    }

    public virtual void FixtureSetup()
    {
    }

    public virtual void FixtureTearDown()
    {
    }

    protected void Register<TInterface>(TInterface concreteType)
    {
        _fixture.Register(() => concreteType);
    }

    protected T MockType<T>()
    {
        return _fixture.Create<T>();
    }
}
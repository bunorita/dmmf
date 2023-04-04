module MathService.Tests

open NUnit.Framework
open MathService

[<TestFixture>]
type TestClass() =
    [<Test>]
    member this.TestMethodPassing() = Assert.True(true)

    [<Test>]
    member this.Add1() =
        let expected = 3
        let actual = MyMath.add1 2
        Assert.That(actual, Is.EqualTo(expected))

// [<SetUp>]
// let Setup () = ()

// [<Test>]
// let Test1 () = Assert.Pass()

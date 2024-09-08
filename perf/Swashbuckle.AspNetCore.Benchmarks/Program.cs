using BenchmarkDotNet.Running;

var switcher = new BenchmarkSwitcher(typeof(Program).Assembly);
switcher.Run(args);

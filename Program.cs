var calls = new List<string>();
var steps = new[] {
 new Step("stock",()=>{calls.Add("reserve");return Task.CompletedTask;},()=>{calls.Add("release");return Task.CompletedTask;}),
 new Step("payment",()=>throw new InvalidOperationException("declined"),()=>Task.CompletedTask)
};
var result = await Saga.Run(steps);
if(result.State!="compensated" || !calls.SequenceEqual(new[]{"reserve","release"}))throw new Exception("compensation invariant");
var ok=await Saga.Run(new[]{steps[0]});if(ok.State!="completed")throw new Exception("success invariant");
var bad=await Saga.Run(new[]{new Step("a",()=>Task.CompletedTask,()=>throw new Exception()),steps[1]});if(bad.State!="manual-intervention")throw new Exception("compensation failure invariant");
Console.WriteLine("PASS: success, reverse compensation and compensation failure");
record Step(string Name,Func<Task> Execute,Func<Task> Compensate);
record Result(string State,IReadOnlyList<string> Events);
static class Saga {
 public static async Task<Result> Run(IEnumerable<Step> steps){var completed=new Stack<Step>();var events=new List<string>();try{foreach(var step in steps){await step.Execute();completed.Push(step);events.Add("completed:"+step.Name);}return new("completed",events);}catch{bool failed=false;while(completed.TryPop(out var step)){try{await step.Compensate();events.Add("compensated:"+step.Name);}catch{failed=true;events.Add("compensation-failed:"+step.Name);}}return new(failed?"manual-intervention":"compensated",events);}}
}

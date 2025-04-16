using System;
using System.Collections.Generic;

namespace Interfaces.PollsService
{
    


public interface ISubject
{
    string State { get; set; }  // Add this property definition
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify();
}

// Concrete Subject class
public class ConcreteSubject : ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    private string state;

    public string State
    {
        get { return state; }
        set
        {
            state = value;
            Notify();
        }
    }

    public void Attach(IObserver observer)
    {
        observers.Add(observer);
    }

    public void Detach(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void Notify()
    {
        foreach (var observer in observers)
        {
            observer.Update();
        }
    }
}


public interface IObserver
{
    void Update();
}


public class ConcreteObserver : IObserver
{
    private string name;
    private ConcreteSubject subject;

    public ConcreteObserver(string name, ConcreteSubject subject)
    {
        this.name = name;
        this.subject = subject;
    }

    public void Update()
    {
        Console.WriteLine($"Observer {name} has been notified with state: {subject.State}");
    }
}

}
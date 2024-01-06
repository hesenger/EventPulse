using EventPulse;

namespace Tests;

public class PersonEventAggregator : IEventAggregator<Person>
{
    public Person Aggregate(Person? person, object evt)
    {
        if (evt is PersonCreatedEvent created)
            return new Person(created);

        if (evt is PersonNameUpdatedEvent updated)
            person!.UpdateName(updated);

        return person!;
    }
}

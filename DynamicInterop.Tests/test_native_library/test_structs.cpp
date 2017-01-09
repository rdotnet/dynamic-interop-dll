
#include "test_structs.h"

namespace testnative
{
	reference_counter::reference_counter()
	{
		count = 1;
	}
	reference_counter::~reference_counter()
	{
		// Some optional message to ::cout?
	}
	void reference_counter::add_reference()
	{
		count++;
	}

	int reference_counter::remove_reference()
	{
		count--;
		return count;
	}
	int reference_counter::reference_count()
	{
		return count;
	}

	dog::dog()
	{
		num_dogs++;
	}

	dog::~dog()
	{
		num_dogs--;
	}
	bool dog::wag_tail(bool b) { return b; }

	int dog::num_dogs(0);
	int owner::num_owners(0);

	owner::owner(dog* d)
	{
		num_owners++;
		this->d = d;
	}

	void owner::say_walk()
	{
		d->wag_tail(true);
	}

	owner::~owner()
	{
		num_owners--;
	}

}


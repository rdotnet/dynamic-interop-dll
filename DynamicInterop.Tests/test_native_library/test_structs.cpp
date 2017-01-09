
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
	}

	dog::~dog()
	{
	}
	bool dog::wag_tail(bool b) { return b; }



	owner::owner(dog* d)
	{
		this->d = d;
	}

	void owner::say_walk()
	{
		d->wag_tail(true);
	}

	owner::~owner()
	{
	}

}


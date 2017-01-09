// test_native_library.cpp : Defines the exported functions for the DLL application.
//

#include "test_native_library.h"

/***********************
* START OF THE API
************************/

void create_date(TEST_DATE_TIME_INFO_PTR start, int year, int month, int day, int hour, int min, int sec)
{
	start->year = year;
	start->month = month;
	start->day = day;
	start->hour = hour;
	start->minute = min;
	start->second = sec;
}

int test_date(TEST_DATE_TIME_INFO_PTR start, int year, int month, int day, int hour, int min, int sec)
{
	if (start->year != year) return 0;
	if (start->month != month) return 0;
	if (start->day != day) return 0;
	if (start->hour != hour) return 0;
	if (start->minute != min) return 0;
	if (start->second != sec) return 0;

	return 1;
}
using namespace testnative;

int get_refcount(reference_counter* obj)
{
	return obj->reference_count();
}

int remove_reference(reference_counter* obj)
{
	return obj->remove_reference();
}

int add_reference(reference_counter* obj)
{
	obj->add_reference();
	return obj->reference_count();
}

TEST_DOG_PTR create_dog()
{
	return new dog();
}

int get_dog_refcount(TEST_DOG_PTR obj)
{
	return get_refcount(obj);
}

int remove_dog_reference(TEST_DOG_PTR obj)
{
	return obj->remove_reference();
}

int add_dog_reference(TEST_DOG_PTR obj)
{
	return add_reference(obj);
}

TEST_OWNER_PTR create_owner(TEST_DOG_PTR d)
{
	return new owner(d);
}

int get_owner_refcount(TEST_OWNER_PTR obj)
{
	return get_refcount(obj);
}

int remove_owner_reference(TEST_OWNER_PTR obj)
{
	return obj->remove_reference();
}

int add_owner_reference(TEST_OWNER_PTR obj)
{
	return add_reference(obj);
}

void say_walk(TEST_OWNER_PTR owner)
{
	owner->say_walk();
}

void release(TEST_COUNTED_PTR obj)
{
	int refCount = obj->remove_reference();
	if (refCount <= 0) delete obj;
}

int num_dogs()
{
	return testnative::dog::num_dogs;
}

int num_owners()
{
	return testnative::owner::num_owners;
}






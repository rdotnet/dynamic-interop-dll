/**
 * \file	test_structs.h
 *
 * \brief	Declares classes and structs used in this test library.
 */

#pragma once

typedef struct _date_time_interop
{
	int year;
	int month;
	int day;
	int hour;
	int minute;
	int second;
} date_time_interop;

typedef struct _interval_interop
{
	date_time_interop start;
	date_time_interop end;
} interval_interop;

namespace testnative
{
	/**
	 * \class	reference_counter
	 *
	 * \brief	A parent class for objects that need reference counting via a C API.
	 * 			Note that the mechanism is very simplistic - not something you would encounter 
	 * 			in a real world code. To begin with, not thread safe. This is for the sake of testing and 
	 * 			didactic purposes.
	 */
	class reference_counter
	{
	protected:
		reference_counter();
	public:
		virtual ~reference_counter();

		void add_reference();
		int remove_reference();
		int reference_count();

	private:
		int count = 1;

	};

	class dog : public reference_counter
	{
	public:
		dog();
		bool wag_tail(bool);
		~dog();

	private:

	};

	class owner : public reference_counter
	{
	public:
		owner(dog* d);
		void say_walk();
		~owner();

	private:
		dog* d;
	};
}


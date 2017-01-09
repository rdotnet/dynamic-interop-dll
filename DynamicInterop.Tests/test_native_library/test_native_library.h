/**
 * \file	test_native_library.h
 *
 * \brief	A C API to test and document how one can use dynamic interop from .NET.
 * 			This test library has two concepts, a dog and its owner. Both have a (simplistic) mechanism for 
 * 			reference counting within C API, something you might encounter in real situations, 
 * 			in which case your .NET wrapper may use these instead of doing it at the .NET level.
 */

#pragma once

// Using patterns in https://msdn.microsoft.com/en-us/library/as6wyhwt(v=vs.100).aspx to mark 
// C interop function as being exported or imported, something necessary with MS cpp tooling.
#ifdef _WIN32
#ifdef USING_TESTLIB_CORE
#define TESTLIB_CORE_DLL_LIB __declspec(dllimport)
#else
#define TESTLIB_CORE_DLL_LIB __declspec(dllexport)
// To prevent warnings such as:
// Warning	C4251	'blah::MyClass' : class 'std::basic_string<char,std::char_traits<char>,std::allocator<char>>' needs to have dll - interface to be used by clients of class 'something'
#pragma warning (disable : 4251)
#endif
#else
#define TESTLIB_CORE_DLL_LIB // nothing
#endif

#define TESTLIB_API  TESTLIB_CORE_DLL_LIB 

#ifdef USING_TESTLIB_CORE
#define TEST_INTERVAL_PTR  void*
#define TEST_DATE_TIME_INFO_PTR  void*
#define TEST_DOG_PTR  void*
#define TEST_OWNER_PTR  void*
#define TEST_COUNTED_PTR  void*
#else
#include "test_structs.h"
#define TEST_INTERVAL_PTR  interval_interop*
#define TEST_DATE_TIME_INFO_PTR  date_time_interop*
#define TEST_DOG_PTR  testnative::dog*
#define TEST_OWNER_PTR  testnative::owner*
#define TEST_COUNTED_PTR  testnative::reference_counter*
#endif

#ifdef __cplusplus
extern "C" {
#endif

	TESTLIB_API void create_date(TEST_DATE_TIME_INFO_PTR start, int year, int month, int day, int hour, int min, int sec);
	TESTLIB_API int test_date(TEST_DATE_TIME_INFO_PTR start, int year, int month, int day, int hour, int min, int sec);

	TESTLIB_API TEST_DOG_PTR create_dog();
	TESTLIB_API int get_dog_refcount(TEST_DOG_PTR obj);
	TESTLIB_API int remove_dog_reference(TEST_DOG_PTR obj);
	TESTLIB_API int add_dog_reference(TEST_DOG_PTR obj);

	TESTLIB_API TEST_OWNER_PTR create_owner(TEST_DOG_PTR d);
	TESTLIB_API int get_owner_refcount(TEST_OWNER_PTR obj);
	TESTLIB_API int remove_owner_reference(TEST_OWNER_PTR obj);
	TESTLIB_API int add_owner_reference(TEST_OWNER_PTR obj);

	TESTLIB_API void say_walk(TEST_OWNER_PTR owner);
	TESTLIB_API void release(TEST_COUNTED_PTR obj);

#ifdef __cplusplus
}
#endif

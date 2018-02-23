/**
 * \file	progress_update.h
 *
 * \brief	A C++ library with a C API to test and document how to set up a callback for a progress update.
 */

#pragma once

// Using patterns in https://msdn.microsoft.com/en-us/library/as6wyhwt(v=vs.100).aspx to mark 
// C interop function as being exported or imported, something necessary with MS cpp tooling.
#ifdef _WIN32
#ifdef USING_PROGRESS_UPDATE_LIB
#define PROGRESS_UPDATE_DLL_LIB __declspec(dllimport)
#else
#define PROGRESS_UPDATE_DLL_LIB __declspec(dllexport)
// To prevent warnings such as:
// Warning	C4251	'blah::MyClass' : class 'std::basic_string<char,std::char_traits<char>,std::allocator<char>>' needs to have dll - interface to be used by clients of class 'something'
#pragma warning (disable : 4251)
#endif
#else
#define PROGRESS_UPDATE_DLL_LIB // nothing
#endif

#define TESTLIB_API  PROGRESS_UPDATE_DLL_LIB 

#ifdef USING_PROGRESS_UPDATE_LIB
#else
#endif

#ifdef __cplusplus
extern "C" {
#endif

	TESTLIB_API bool do_long_lived_task();
	TESTLIB_API void register_progress_update_callback(const void* callback);

#ifdef __cplusplus
}
#endif

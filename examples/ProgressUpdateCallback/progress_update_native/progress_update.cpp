// progress_update.cpp : Defines the exported functions for the DLL application.
//

#include "progress_update.h"
#include "update_handlers.h"

using std::string;
using progress_update_lib::callback_register;

callback_register::update_callback callback_register::callback = nullptr;

void callback_register::notify_progress_update(double progressValue)
{
	if (callback != nullptr)
		(*callback)(progressValue);
}

void callback_register::register_update_callback(const void* callback, bool allow_override)
{
	if (callback == nullptr)
		callback_register::callback = nullptr;
	else
	{
		if (callback_register::callback != nullptr && !allow_override)
			throw std::logic_error("moirai::callback_register already has an error handler set up!");
		else
			callback_register::callback = (callback_register::update_callback) callback;
	}
}

bool callback_register::has_callback_registered()
{
	return (callback != nullptr);
}

// below are the API functions exported 

#include <iostream>       // std::cout, std::endl
#include <thread>         // std::this_thread::sleep_for
#include <chrono>         // std::chrono::seconds


bool do_long_lived_task()
{
	for (int i = 1; i<10; ++i) {
		std::this_thread::sleep_for(std::chrono::seconds(1));
		callback_register::notify_progress_update(i / 10.0);
	}
	return true;
}

void register_progress_update_callback(const void* callback)
{
	callback_register::register_update_callback(callback);
}

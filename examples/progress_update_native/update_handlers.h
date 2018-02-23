#pragma once

#include <string>
using std::string;

namespace progress_update_lib {
	class callback_register
	{
	public:
		typedef void(*update_callback)(double progressValue);
		static void notify_progress_update(double progressValue);
		static void register_update_callback(const void* callback, bool allow_override = false);
		static bool has_callback_registered();
	private:
		static update_callback callback;
	};
}

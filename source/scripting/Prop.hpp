#pragma once

#include "Entity.hpp"
#include "Native.hpp"

namespace GTA
{
	public ref class Prop sealed : public Entity
	{
	public:
		Prop(int handle) : Entity(handle), _handle(handle)
		{
		}

		void SetTrafficLight(int state)
		{
			Native::Function::Call(Native::Hash::SET_ENTITY_TRAFFICLIGHT_OVERRIDE, _handle, state);
		}

	private:
		int _handle;
	};
}
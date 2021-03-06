/* Copyright 2013 David Axmark

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

/*! \addtogroup NativeUILib
 *  @{
 */

/**
 *  @defgroup NativeUILib Native UI Library
 *  @{
 */

/**
 * @file WidgetManager.h
 * @author Mikael Kindborg
 *
 * \brief The WidgetManager manages widget events and dispatches
 * them to the target widgets.
 */

#ifndef NATIVEUI_WIDGETMANAGER_H_
#define NATIVEUI_WIDGETMANAGER_H_

#include <ma.h>

#include <MAUtil/String.h>
#include <MAUtil/Map.h>
#include <MAUtil/Environment.h>

#include <IX_WIDGET.h>

#include "WidgetEventListener.h"
#include "Widget.h"

namespace NativeUI
{

    /**
     * \brief Class that handles widget events.
     */
    class WidgetManager : public MAUtil::CustomEventListener
    {
    public:
        /**
         * Destructor.
         */
        virtual ~WidgetManager();

        /**
         * Return the single instance of this class.
         */
        static WidgetManager* getInstance();

        /**
         * Destroy the single instance of this class.
         * Call this method only when the application will exit.
         */
        static void destroyInstance();

        /**
         * Implementation of CustomEventListener interface.
         * This method will get called whenever there is a
         * widget event generated by the system.
         * @param event The new received event.
         */
        virtual void customEvent(const MAEvent& event);

        /**
         * Add a widget to the map that holds widgets.
         * The widget will receive custom events.
         * @param widgetHandle The handle of the widget that needs to be registered.
         * @param widget The widget that needs to be registered.
         * The ownership of the widget is not passed to this method.
         */
        virtual void registerWidget(MAHandle widgetHandle, Widget* widget);

        /**
         * Remove a widget from the map that holds widgets.
         * The widget will not receive custom events.
         * @param widgetHandle The handle of the widget that needs to be unregistered.
         */
        virtual void unregisterWidget(MAHandle widgetHandle);

    protected:
        /**
         * Constructor is protected since this is a singleton.
         * (subclasses can still create instances).
         */
        WidgetManager();

    private:
        /**
         * Error handling for devices that do not support NativeUI.
         * Here we throw a panic if NativeUI is not supported.
         * @param result The widget handle.
         */
        void checkNativeUISupport(MAHandle result) const;

        /**
         * The single instance of this class.
         */
        static WidgetManager* sInstance;

        /**
         * Dictionary of widgets identified by widget handle.
         */
        MAUtil::Map<MAHandle, Widget*> mWidgetMap;
    };

} // namespace NativeUI

#endif

/*! @} */

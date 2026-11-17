// The catalog: every event the application records, with its parameters. The service
// accepts only a name listed here, so an unknown name does not compile.
//
// Project events are named <object>_<action in the past tense>, lowercase with
// underscores (tour_published). Google Analytics' recommended names (page_view,
// sign_up, login) are kept, so its reports recognise them.
//
// No event carries a name, an email or any other personal data. The service adds the
// user's role to every event by itself.
export interface AnalyticsEvents {
  page_view: { page_location: string };
  sign_up: Record<string, never>;
  login: Record<string, never>;
  tour_created: Record<string, never>;
  tour_published: Record<string, never>;
}

export type AnalyticsEventName = keyof AnalyticsEvents;

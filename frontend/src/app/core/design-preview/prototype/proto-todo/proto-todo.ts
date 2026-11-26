import { Component, input } from '@angular/core';

// PROTOTYPE. Stands in for a screen of the new navigation that is not drawn yet; the route's data
// says which screen and which scenario will fill it.
@Component({
  selector: 'app-proto-todo',
  template: `
    <div class="state">
      <strong>{{ title() }}</strong>
      <span>{{ plan() }}</span>
      <span class="meta">Ekran još nije u prototipu.</span>
    </div>
  `,
})
export class ProtoTodo {
  readonly title = input('');
  readonly plan = input('');
}

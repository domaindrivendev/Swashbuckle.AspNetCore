const fs = require('fs');
const path = require('path');

const sourceDir = 'node_modules/swagger-ui-dist';

const filenames = fs.readdirSync(sourceDir);

filenames.forEach((filename) => {
    if (!filename.endsWith('.js') && !filename.endsWith('.css')) return;

    const filepath = path.join(sourceDir, filename);
    const contents = fs.readFileSync(filepath, 'utf-8');

    const updatedContents = contents.replace(/# sourceMappingURL=(.+?\.map)/g, '');
    fs.writeFileSync(filepath, updatedContents, 'utf-8');
});